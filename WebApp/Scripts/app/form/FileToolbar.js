/***
 
.x-attachment-btn {
   border-color: #6262e5 !important;
}

 ***/ //Required CSS

//定義AttachmentToolbar元件
Ext.define('WEBAPP.form.FileToolbar', {
    extend: 'Ext.toolbar.Toolbar',

    /*******************************
    ** 公有屬性，可在config設定
    ********************************/
    canUpload: true, //是否可以上傳檔案
    canDelete: true, //是否可以刪除檔案
    maxFiles: 0, //最多存幾個檔案, 0表示無限制
    autoGenerateUploadKey: true,
    browseButtonText: '瀏覽',
    clearButtonText: '清除',
    uploadButtonText: '上傳',
    deleteButtonText: '刪檔',

    /*******************************
    ** 私有屬性，勿設定或使用
    ********************************/

    //AttachmentToolbar元件初始化
    initComponent: function () {
        Ext.apply(this, this.getDefaultConfig());
        this.callParent(arguments);
    },

    getDefaultConfig: function () {
        return {
            hidden: true,
            items: [
                {
                    xtype: 'panel',
                    hidden: true,
                    html: '<input type="file" class="fm" style="display:none;" onchange="javascript:FileUtil.onFileChange(this);" multiple /> '
                }, {
                    xtype: 'panel',
                    hidden: true,
                    html: '<input type="file" class="fs" style="display:none;" onchange="javascript:FileUtil.onFileChange(this);" /> '
                },
                {
                    cls: 'x-attachment-btn',
                    itemId: 'btnFileBrowse',
                    browseButtonText: this.browseButtonText,
                    clearButtonText: this.clearButtonText,
                    text: this.browseButtonText,
                    hidden: !this.canUpload,
                    toolbar: this,
                    handler: function () {
                        var toolbar = this.toolbar;
                        if (this.getText() == this.browseButtonText)
                            toolbar.getFileField().click();
                        else if (this.getText() == this.clearButtonText) {
                            var selectedNonUploadItems = toolbar.store.queryBy(
                                function (record) {
                                    return !record.get('DN');
                                });
                            var len = selectedNonUploadItems.length;
                            for (i = 0; i < len; i++)
                                toolbar.store.remove(selectedNonUploadItems.items[i]);
                            toolbar.getFileField().value = '';
                            toolbar.setUploadButtonQty(0);
                        }
                    }
                }, {
                    cls: 'x-attachment-btn',
                    itemId: 'btnFileUpload',
                    uploadButtonText: this.uploadButtonText,
                    text: this.uploadButtonText,
                    disabled: true,
                    hidden: !this.canUpload,
                    toolbar: this,
                    handler: function () {
                        var gridUpload = this.up('grid');
                        var toolbar = this.toolbar;
                        var parent = toolbar.parent;
                        if (toolbar.maxFiles > 0 &&
                            toolbar.store.getCount() > toolbar.maxFiles) {
                            Ext.Msg.show({
                                title: '提醒訊息',
                                msg: '上傳檔案數不得超過' + toolbar.maxFiles + '個',
                                buttons: Ext.Msg.OK,
                                icon: Ext.MessageBox.WARNING,
                                cls: 'warnMsg'
                            });
                        }
                        else {
                            gridUpload.showMask('上傳中...');
                            FileUtil.UploadFiles({
                                isUploadKeyConfirmed: parent.getUploadKeyConfirmed(),
                                uploadKey: parent.getValue(),
                                fileField: toolbar.getFileField(),
                                doneCallback: function (args, result) {
                                    parent.setValue(result.newGuid);
                                    parent.setUploadKeyConfirmed(result.ukConfirmed != "N");

                                    var selectedUploadedItems = gridUpload.getStore().queryBy(
                                        function (record) {
                                            return !record.get('DN');
                                        });
                                    Ext.each(selectedUploadedItems.items, function (item, index) {
                                        item.set({ SL: false, DN: true });
                                    });
                                    toolbar.getFileField().value = '';
                                    toolbar.setUploadButtonQty(0);
                                    toolbar.getUploadButton().setDisabled(true);
                                    toolbar.getDeleteButton().setDisabled(true);
                                    gridUpload.getStore().reload();
                                    gridUpload.hideMask();

                                    Ext.Msg.show({
                                        title: '提醒訊息',
                                        msg: '上傳完成',
                                        buttons: Ext.Msg.OK,
                                        icon: Ext.MessageBox.WARNING,
                                        cls: 'warnMsg'
                                    });
                                }
                            });
                        }
                    }
                }, {
                    cls: 'x-attachment-btn',
                    itemId: 'btnFileDelete',
                    text: this.deleteButtonText,
                    disabled: true,
                    hidden: !this.canDelete,
                    toolbar: this,
                    handler: function () {
                        var gridUpload = this.up('grid');
                        var toolbar = this.toolbar;
                        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                            if (btn === 'yes') {
                                gridUpload.showMask("刪除中...");
                                var selectedFiles = toolbar.store.queryBy(
                                    function (record) {
                                        return record.get('DN') && record.get('SL');
                                    });
                                var len = selectedFiles.length;
                                var selectedFileGuids = new Array();
                                for (i = 0; i < len; i++)
                                    selectedFileGuids.push(selectedFiles.items[i].get('FG'));
                                FileUtil.DeleteFiles({
                                    selectedFileGuids: selectedFileGuids,
                                    doneCallback: function () {
                                        toolbar.store.load(
                                            {
                                                callback: function () {
                                                    toolbar.setDeleteButtonStatus();
                                                }
                                            });
                                        gridUpload.hideMask();
                                    }
                                });
                            }
                        }
                        );
                    }
                }
            ],
            
            onFileChange: function (args) {
                var target = args.fileEl;
                var fileExtValid = args.fileExtValid;

                if (target.files.length > 0) {
                    if (fileExtValid) {
                        var toolbar = this;
                        this.setUploadButtonQty(target.files.length);
                        Ext.each(target.files, function (item, index) {
                            toolbar.store.insert(0, { SL: true, DN: false, FN: item.name, FC: new Date() });
                        });
                    }
                    else {
                        this.setUploadButtonQty(0);
                    }
                }
                this.setDeleteButtonStatus();
            },
            getFileField: function () {
                //var _ffCls = (this.getAttachmentField().maxFiles == 1) ? 'fs' : 'fm';
                var _ffCls = (this.maxFiles == 1) ? 'fs' : 'fm';
                var _ffEl = $('#' + this.id + ' input[class=' + _ffCls + ']')[0];
                _ffEl.reportId = this.id;
                return _ffEl;
            },
            getBrowseButton: function () {
                return this.down('#btnFileBrowse');
            },
            getUploadButton: function () {
                return this.down('#btnFileUpload');
            },
            getDeleteButton: function () {
                return this.down('#btnFileDelete');
            },
            getUploadKey: function () {
                return this.parent.getValue();
            },
            getIsUploadKeyConfirmed: function () {
                return this.parent.isUploadKeyConfirmed;
            },
            setUploadButtonQty: function (qty) {
                var browseButton = this.getBrowseButton();
                var uploadButton = this.getUploadButton();
                if (qty > 0) {
                    uploadButton.setText(uploadButton.uploadButtonText +
                        '(<span style="color:red;">' + qty + '</span>)');
                    browseButton.setText(browseButton.clearButtonText);
                    browseButton.addCls('x-attachment-btn-clear');
                }
                else {
                    uploadButton.setText(uploadButton.uploadButtonText);
                    browseButton.setText(browseButton.browseButtonText);
                    browseButton.removeCls('x-attachment-btn-clear');
                }
                uploadButton.setDisabled(qty == 0);
            },
            setDeleteButtonStatus: function () {
                var selectedItems = this.store.queryBy(
                    function (record) {
                        return record.get('DN') && record.get('SL');
                    });
                this.getDeleteButton().setDisabled(selectedItems.length == 0);
            },
            resetButtonStatus: function () {
                this.setUploadButtonQty(0);
                this.setDeleteButtonStatus();
            }
        };
    }
});