import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError } from 'rxjs';
import { throwError } from 'rxjs';

type ValidationErrorResponse = {
    message?: string;
    errors?: Array<{ field: string; messages: string[] }>;
};

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    return next(req).pipe(
        catchError((err: HttpErrorResponse) => {
            let message = 'An unexpected error occurred';
            let suppressGlobalAlert = false;

            // Network or CORS errors often appear as status 0
            if (err.status === 0) {
                message = 'Network error. Please check your internet connection.';
            } else {
                const body: any = err.error;
                const backendMessage: string | undefined = body?.message;

                switch (err.status) {
                    case 400: {
                        // Attempt to parse validation errors
                        const val = body as ValidationErrorResponse | undefined;
                        if (val?.errors && Array.isArray(val.errors) && val.errors.length > 0) {
                            const details = val.errors
                                .map(e => `${e.field}: ${e.messages.join(', ')}`)
                                .join('\n');
                            message = `${val.message ?? 'Validation failed'}\n${details}`;
                            // Let components render field-level errors; avoid noisy alerts
                            suppressGlobalAlert = true;
                        } else {
                            message = backendMessage ?? 'Bad request';
                        }
                        break;
                    }
                    case 409: {
                        message = backendMessage ?? 'Conflict';
                        // Validation-like UX for conflicts: avoid global alert if component can show it
                        suppressGlobalAlert = true;
                        break;
                    }
                    case 401: {
                        message = backendMessage ?? 'Authentication required or token invalid';
                        break;
                    }
                    case 403: {
                        message = backendMessage ?? 'You do not have permission to perform this action';
                        break;
                    }
                    case 404: {
                        message = backendMessage ?? 'Resource not found';
                        break;
                    }
                    default: {
                        message = backendMessage ?? message;
                        break;
                    }
                }
            }

            // Show alert to the user unless we want components to handle it
            if (!suppressGlobalAlert) {
                try {
                    window.alert(message);
                } catch (_) {
                    // In non-browser environments, just ignore
                }
            }

            return throwError(() => err);
        })
    );
};


