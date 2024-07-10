export interface IEntityPermissions {
  allowCreating?: boolean;
  allowEditing?: boolean;
  allowDeleting?: boolean;
}

export const entityFullPermissions: IEntityPermissions = {
  allowCreating: true,
  allowEditing: true,
  allowDeleting: true,
};
